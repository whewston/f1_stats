# F1 predictor

Offline batch inference. Pulls features from the public API, ranks the field with
a transparent weighted baseline, and submits predictions to the admin endpoint.

```powershell
cd predictor
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
Copy-Item .env.example .env     # edit if your API base / key differ

python predict.py --dry-run     # preview, submits nothing
python predict.py               # predicts the next race and submits
python predict.py --year 2026 --round 11
```