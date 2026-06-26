"""F1 race-result predictor (offline batch inference).

Pulls features from the public F1 Stats API, ranks the field with a transparent
weighted baseline, generates human-readable reasons for each prediction, and
submits the result to the protected admin endpoint.

The model is interpretable by construction: every driver's score is an additive
blend of a few named factors, so the same numbers that produce the ranking also
produce the explanation. Swapping in a trained model later only changes
`build_predictions` — feature gathering and submission stay put.
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
MODEL_VERSION = "baseline-v0.1"
RECENT_WINDOW = 3                 # races of recent form to weigh

# scoring weights (sum to 1.0) — these *are* the model
W_STRENGTH = 0.40                 # season championship position
W_FORM     = 0.40                 # recent finishing positions
W_CIRCUIT  = 0.20                 # past wins at this circuit


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
    """driverId -> recent finishing positions, most-recent-first."""
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
    """driverId -> wins at this circuit, from the preview endpoint."""
    prev = get(f"/api/seasons/{year}/races/{rnd}/preview")
    return {w["driverId"]: w["wins"] for w in (prev or {}).get("topWinners", [])}


def reasons_for(x):
    """Top human-readable factors behind a driver's ranking (XAI)."""
    out = []
    pos = x["champPos"]
    if pos == 1:
        out.append("Leads the championship")
    elif pos <= 3:
        out.append(f"P{pos} in the title race")

    wh = x["winsHere"]
    if wh > 0:
        out.append(f"Won here {wh} time{'s' if wh > 1 else ''} before")

    recent = x["recent"]            # most-recent-first
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


def build_predictions(year, rnd):
    field = get(f"/api/seasons/{year}/standings/drivers")
    if not field:
        sys.exit(f"No standings for {year}; nothing to predict.")
    n = len(field)
    form = recent_form(year, rnd)
    wins_here = circuit_wins(year, rnd)

    scored = []
    for s in field:
        did, pos = s["driverId"], s["position"]
        recent = form.get(did, [])
        f_strength = (n - pos + 1) / n
        f_form = (mean(21 - p for p in recent) / 20) if recent else 0.5   # neutral if unknown
        wh = wins_here.get(did, 0)
        f_circuit = min(wh, 5) / 5
        score = W_STRENGTH * f_strength + W_FORM * f_form + W_CIRCUIT * f_circuit
        scored.append({"driverId": did, "score": score, "champPos": pos,
                       "recent": recent, "winsHere": wh})

    scored.sort(key=lambda x: x["score"], reverse=True)

    beta = 6.0                       # softmax sharpness for win probability
    exps = [math.exp(beta * x["score"]) for x in scored]
    total = sum(exps) or 1.0

    return [{
        "driverId": x["driverId"],
        "predictedPosition": i,
        "winProbability": round(e / total, 4),
        "reasons": reasons_for(x),
    } for i, (x, e) in enumerate(zip(scored, exps), start=1)]


def submit(year, rnd, predictions):
    r = requests.post(
        f"{API_BASE}/admin/predictions/{year}/{rnd}",
        headers={"X-Admin-Key": ADMIN_KEY},
        json={"modelVersion": MODEL_VERSION, "predictions": predictions},
        timeout=20,
    )
    if r.status_code != 200:
        sys.exit(f"Submit failed [{r.status_code}]: {r.text}")
    return r.json()


def main():
    ap = argparse.ArgumentParser(description="Predict an F1 race and submit it.")
    ap.add_argument("--year", type=int)
    ap.add_argument("--round", type=int)
    ap.add_argument("--dry-run", action="store_true", help="print, don't submit")
    args = ap.parse_args()

    year, rnd = resolve_race(args.year, args.round)
    print(f"Predicting {year} round {rnd}  (model {MODEL_VERSION})\n")
    preds = build_predictions(year, rnd)

    for p in preds[:10]:
        why = ("  — " + "; ".join(p["reasons"])) if p["reasons"] else ""
        print(f"  P{p['predictedPosition']:<2} {p['driverId']:<18} "
              f"{p['winProbability'] * 100:4.1f}%{why}")

    if args.dry_run:
        print("\nDry run — nothing submitted.")
        return
    res = submit(year, rnd, preds)
    print(f"\nSubmitted {res['count']} predictions for {year} round {rnd}.")


if __name__ == "__main__":
    main()