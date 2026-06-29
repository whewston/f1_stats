"""F1 race-result predictor (offline batch inference).

Two phases share one contract:
  - pre_qualifying:  form + championship + circuit history (inherently fuzzy)
  - post_qualifying: same, but grid position dominates (much sharper)

The model is interpretable by construction: every driver's score is an additive
blend of named factors, so the numbers that rank a driver also explain them.
"""
import argparse
import math
import os
import sys
from collections import defaultdict

import requests
from dotenv import load_dotenv

load_dotenv()
API_BASE = os.environ.get("F1_API_BASE", "http://localhost:8080").rstrip("/")
ADMIN_KEY = os.environ.get("F1_ADMIN_KEY", "dev-secret")
RECENT_WINDOW = 3

# weight profiles — these *are* the model. grid only exists post-qualifying.
PRE_WEIGHTS  = {"strength": 0.40, "form": 0.40, "circuit": 0.20, "grid": 0.00}
POST_WEIGHTS = {"strength": 0.15, "form": 0.20, "circuit": 0.10, "grid": 0.55}
BETA = 6.0   # softmax sharpness for win probability


def get(path):
    r = requests.get(f"{API_BASE}{path}", timeout=20)
    if r.status_code == 404:
        return None
    r.raise_for_status()
    return r.json()


def mean(xs):
    xs = list(xs)
    return sum(xs) / len(xs) if xs else 0.0


def resolve_race(year, rnd):
    if year and rnd:
        return year, rnd
    nxt = get("/api/races/next")
    if not nxt:
        sys.exit("No upcoming race found — ingest the current season first.")
    return nxt["season"], nxt["round"]


def recent_form(year, upto_round):
    races = get(f"/api/seasons/{year}/races") or []
    done = sorted((r for r in races if r["round"] < upto_round),
                  key=lambda r: r["round"], reverse=True)
    form = defaultdict(list)
    for r in done[:RECENT_WINDOW]:
        res = get(f"/api/seasons/{year}/races/{r['round']}/results")
        if not res:
            continue
        for row in res["results"]:
            if row["position"] is not None:
                form[row["driverId"]].append(row["position"])
    return form


def circuit_wins(year, rnd):
    prev = get(f"/api/seasons/{year}/races/{rnd}/preview")
    return {w["driverId"]: w["wins"] for w in (prev or {}).get("topWinners", [])}


def qualifying_grid(year, rnd):
    """driverId -> grid (qualifying) position, or {} if no quali yet."""
    q = get(f"/api/seasons/{year}/races/{rnd}/qualifying")
    return {r["driverId"]: r["position"] for r in (q or {}).get("rows", [])}


def resolve_phase(arg, year, rnd):
    if arg == "pre":
        return "pre_qualifying"
    if arg == "post":
        return "post_qualifying"
    return "post_qualifying" if qualifying_grid(year, rnd) else "pre_qualifying"


def reasons_for(x, phase):
    out = []
    if phase == "post_qualifying":
        g = x.get("grid")
        if g == 1:
            out.append("Starts on pole")
        elif g:
            out.append(f"Starts P{g}")

    pos = x["champPos"]
    if pos == 1:
        out.append("Leads the championship")
    elif pos <= 3:
        out.append(f"P{pos} in the title race")

    wh = x["winsHere"]
    if wh > 0:
        out.append(f"Won here {wh} time{'s' if wh > 1 else ''} before")

    recent = x["recent"]
    streak = 0
    for p in recent:
        if p <= 3:
            streak += 1
        else:
            break
    if streak >= 2:
        out.append(f"On a {streak}-race podium streak")
    else:
        pods = sum(1 for p in recent if p <= 3)
        if pods >= 2:
            out.append(f"{pods} podiums in the last {len(recent)} races")
        elif recent:
            avg = round(mean(recent))
            if avg <= 6:
                out.append(f"Averaging P{avg} over the last {len(recent)} races")

    return out[:3]


def build_predictions(year, rnd, phase):
    field = get(f"/api/seasons/{year}/standings/drivers")
    if not field:
        sys.exit(f"No standings for {year}; nothing to predict.")
    n = len(field)
    form = recent_form(year, rnd)
    wins_here = circuit_wins(year, rnd)
    grid = qualifying_grid(year, rnd) if phase == "post_qualifying" else {}

    if phase == "post_qualifying" and not grid:
        sys.exit("Post-qualifying requested but no qualifying data for this round yet.")

    w = POST_WEIGHTS if phase == "post_qualifying" else PRE_WEIGHTS

    scored = []
    for s in field:
        did, pos = s["driverId"], s["position"]
        recent = form.get(did, [])
        f_strength = (n - pos + 1) / n
        f_form = (mean(21 - p for p in recent) / 20) if recent else 0.5
        wh = wins_here.get(did, 0)
        f_circuit = min(wh, 5) / 5
        g = grid.get(did)
        f_grid = ((n - g + 1) / n) if g else 0.5
        score = (w["strength"] * f_strength + w["form"] * f_form
                 + w["circuit"] * f_circuit + w["grid"] * f_grid)
        scored.append({"driverId": did, "score": score, "champPos": pos,
                       "recent": recent, "winsHere": wh, "grid": g})

    scored.sort(key=lambda x: x["score"], reverse=True)
    exps = [math.exp(BETA * x["score"]) for x in scored]
    total = sum(exps) or 1.0

    return [{
        "driverId": x["driverId"],
        "predictedPosition": i,
        "winProbability": round(e / total, 4),
        "reasons": reasons_for(x, phase),
    } for i, (x, e) in enumerate(zip(scored, exps), start=1)]


def submit(year, rnd, phase, model_version, predictions):
    r = requests.post(
        f"{API_BASE}/admin/predictions/{year}/{rnd}",
        headers={"X-Admin-Key": ADMIN_KEY},
        json={"modelVersion": model_version, "phase": phase, "predictions": predictions},
        timeout=20,
    )
    if r.status_code != 200:
        sys.exit(f"Submit failed [{r.status_code}]: {r.text}")
    return r.json()


def main():
    ap = argparse.ArgumentParser(description="Predict an F1 race and submit it.")
    ap.add_argument("--year", type=int)
    ap.add_argument("--round", type=int)
    ap.add_argument("--phase", choices=["auto", "pre", "post"], default="auto",
                    help="auto picks post if qualifying exists, else pre")
    ap.add_argument("--dry-run", action="store_true", help="print, don't submit")
    args = ap.parse_args()

    year, rnd = resolve_race(args.year, args.round)
    phase = resolve_phase(args.phase, year, rnd)
    model_version = f"baseline-{'post' if phase == 'post_qualifying' else 'pre'}-quali-v0.2"

    print(f"Predicting {year} round {rnd}  ({phase}, model {model_version})\n")
    preds = build_predictions(year, rnd, phase)

    for p in preds[:10]:
        why = ("  — " + "; ".join(p["reasons"])) if p["reasons"] else ""
        print(f"  P{p['predictedPosition']:<2} {p['driverId']:<18} "
              f"{p['winProbability'] * 100:4.1f}%{why}")

    if args.dry_run:
        print("\nDry run — nothing submitted.")
        return
    res = submit(year, rnd, phase, model_version, preds)
    print(f"\nSubmitted {res['count']} predictions ({phase}) for {year} round {rnd}.")


if __name__ == "__main__":
    main()