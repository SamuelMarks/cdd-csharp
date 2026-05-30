#!/usr/bin/env python3
import sys
import os
import re

# Forbidden patterns that cause stack overflows in WASM due to deep recursion
FORBIDDEN_PATTERNS = [
    re.compile(r'NormalizeWhitespace'),
    re.compile(r'\.DescendantNodes\('),
    re.compile(r'\.DescendantTokens\(')
]

# Files to exclude from linting
EXCLUDED_FILES = {
    'WasmSafeRoslyn.cs',
    'WasmSafeFormatter.cs'
}

def check_file(filepath):
    basename = os.path.basename(filepath)
    if basename in EXCLUDED_FILES:
        return False

    # Ignore obj and bin folders
    normalized_path = filepath.replace('\\', '/')
    if '/obj/' in normalized_path or '/bin/' in normalized_path:
        return False

    if not filepath.endswith('.cs'):
        return False

    has_error = False
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            for line_num, line in enumerate(f, 1):
                for pattern in FORBIDDEN_PATTERNS:
                    if pattern.search(line):
                        print(f"ERROR: Found forbidden pattern '{pattern.pattern}' in {filepath}:{line_num}")
                        print("This causes stack overflows in WASM.")
                        print("Please use WasmSafeFormatter.Format(node) or GetDescendantNodesSafe() instead.")
                        has_error = True
    except Exception as e:
        print(f"Failed to read {filepath}: {e}")

    return has_error

def main():
    print("Linting for WASM-breaking recursive methods...")
    has_errors = False

    # If arguments are provided (e.g., via pre-commit passing filenames)
    if len(sys.argv) > 1:
        files_to_check = sys.argv[1:]
    else:
        # Fallback to scanning the src directory
        files_to_check = []
        src_dir = os.path.join(os.getcwd(), 'src')
        if os.path.exists(src_dir):
            for root, _, files in os.walk(src_dir):
                for file in files:
                    files_to_check.append(os.path.join(root, file))

    for filepath in files_to_check:
        if check_file(filepath):
            has_errors = True

    if has_errors:
        sys.exit(1)

    print("Linting passed.")

if __name__ == '__main__':
    main()
