#!/usr/bin/env python3
import os
import sys
import subprocess

if os.environ.get("RUN_SLOW_TESTS") not in ("1", "true", "True"):
    print(f"Skipped {' '.join(sys.argv[1:])} (RUN_SLOW_TESTS not set)")
    sys.exit(0)

sys.exit(subprocess.run(sys.argv[1:]).returncode)
