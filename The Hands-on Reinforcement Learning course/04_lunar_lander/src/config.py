import os
import pathlib
root_dir = pathlib.Path(__file__).parent.resolve().parent

SAVED_AGENTS_DIR = root_dir / 'saved_agents'
TENSORBOARD_LOG_DIR = root_dir / 'tensorboard_logs'
# OPTUNA_DB = root_dir / 'optuna.db'
# DATA_SUPERVISED_ML = root_dir / 'data_supervised_ml'
# MLFLOW_RUNS_DIR = root_dir / 'mlflow_runs'

if not SAVED_AGENTS_DIR.exists():
    os.makedirs(SAVED_AGENTS_DIR)

if not TENSORBOARD_LOG_DIR.exists():
    os.makedirs(TENSORBOARD_LOG_DIR)

# if not DATA_SUPERVISED_ML.exists():
#     os.makedirs(DATA_SUPERVISED_ML)
#
# if not MLFLOW_RUNS_DIR.exists():
#     os.makedirs(MLFLOW_RUNS_DIR)