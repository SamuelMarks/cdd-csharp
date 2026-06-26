#!/usr/bin/env python3
import sys
import subprocess
import shutil
import os

def main():
    if len(sys.argv) < 2:
        sys.exit(1)

    cmd = sys.argv[1]
    args = sys.argv[2:]

    if shutil.which(cmd):
        try:
            sys.exit(subprocess.run([cmd] + args).returncode)
        except Exception as e:
            print(f"Error executing {cmd}: {e}")
            sys.exit(1)

    has_docker = shutil.which("docker") and subprocess.run(["docker", "info"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL).returncode == 0

    if has_docker:
        cwd = os.getcwd()
        if cmd in ("python3", "python"):
            docker_cmd = ["docker", "run", "--rm", "-v", f"{cwd}:/app", "-w", "/app", "python:3-slim", "python"] + args
        elif cmd == "dotnet":
            docker_cmd = ["docker", "run", "--rm", "-v", f"{cwd}:/app", "-w", "/app", "mcr.microsoft.com/dotnet/sdk:8.0", "dotnet"] + args
        elif cmd == "java":
            docker_cmd = ["docker", "run", "--rm", "-v", f"{cwd}:/app", "-w", "/app", "eclipse-temurin:17", "java"] + args
        else:
            print(f"No docker fallback defined for {cmd}")
            sys.exit(1)

        sys.exit(subprocess.run(docker_cmd).returncode)
    else:
        print(f"Error: {cmd} is not installed and Docker is not available.")
        sys.exit(1)

if __name__ == "__main__":
    main()
