#!/usr/bin/env python3
import sys
import re
import os

def main():
    if len(sys.argv) < 2:
        print("Usage: bump_version.py <new_version>")
        sys.exit(1)

    new_version = sys.argv[1].strip()
    # Remove any leading 'v' if present
    if new_version.startswith('v'):
        new_version = new_version[1:]

    print(f"Bumping version to {new_version}")

    # 1. Update haos-jellyfin/config.yaml
    config_path = "haos-jellyfin/config.yaml"
    if os.path.exists(config_path):
        with open(config_path, "r") as f:
            content = f.read()
        # Find version: "..." or version: ...
        new_content = re.sub(r'^(version:\s*)"?[0-9.]+"?', f'\\1"{new_version}"', content, flags=re.MULTILINE)
        with open(config_path, "w") as f:
            f.write(new_content)
        print(f"Updated {config_path}")

    # 2. Update haos-jellyfin/addon.yaml if exists
    addon_path = "haos-jellyfin/addon.yaml"
    if os.path.exists(addon_path):
        with open(addon_path, "r") as f:
            content = f.read()
        new_content = re.sub(r'^(version:\s*)"?[0-9.]+"?', f'\\1"{new_version}"', content, flags=re.MULTILINE)
        with open(addon_path, "w") as f:
            f.write(new_content)
        print(f"Updated {addon_path}")

    # 3. Update haos-jellyfin/CHANGELOG.md
    changelog_path = "haos-jellyfin/CHANGELOG.md"
    if os.path.exists(changelog_path):
        with open(changelog_path, "r") as f:
            lines = f.readlines()

        # We want to insert the new version changelog under '# Changelog'
        new_lines = []
        inserted = False
        for line in lines:
            new_lines.append(line)
            if line.strip() == "# Changelog" and not inserted:
                new_lines.append("\n")
                new_lines.append(f"## {new_version}\n")
                new_lines.append(f"- Automated version bump to {new_version}.\n")
                inserted = True

        with open(changelog_path, "w") as f:
            f.writelines(new_lines)
        print(f"Updated {changelog_path}")

    # 4. Update .github/ISSUE_TEMPLATE/issue report.yml
    issue_template_path = ".github/ISSUE_TEMPLATE/issue report.yml"
    if os.path.exists(issue_template_path):
        with open(issue_template_path, "r") as f:
            content = f.read()

        # We want to find:
        #   - type: dropdown
        #     id: version
        #     attributes:
        #       label: Jellyfin Server version
        #       description: What version of Jellyfin are you using?
        #       options:
        #         - <first_version>
        # And insert the new version right before the <first_version>
        # Let's use a state machine to find the correct options section.
        lines = content.splitlines()
        new_lines = []
        in_version_dropdown = False
        in_attributes = False
        in_options = False
        inserted = False

        for line in lines:
            stripped = line.strip()
            if stripped == "id: version":
                in_version_dropdown = True
            elif in_version_dropdown and stripped.startswith("- type:"):
                # Left the version dropdown section
                in_version_dropdown = False
                in_attributes = False
                in_options = False
            elif in_version_dropdown and stripped == "attributes:":
                in_attributes = True
            elif in_version_dropdown and in_attributes and stripped == "options:":
                in_options = True
                new_lines.append(line)
                if not inserted:
                    # Insert the new version as the first option
                    # We preserve the indentation of the options line
                    indent = len(line) - len(line.lstrip())
                    new_lines.append(" " * (indent + 2) + f"- {new_version}")
                    inserted = True
                continue

            new_lines.append(line)

        with open(issue_template_path, "w") as f:
            f.write("\n".join(new_lines) + "\n")
        print(f"Updated {issue_template_path}")

if __name__ == "__main__":
    main()
