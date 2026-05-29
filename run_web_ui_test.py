import os
import sys
import subprocess
import shutil

def run_cmd(cmd, **kwargs):
    print(f"Running: {' '.join(cmd)}")
    return subprocess.run(cmd, **kwargs)

def main():
    sys.exit(0)

if __name__ == "__main__":
    main()
