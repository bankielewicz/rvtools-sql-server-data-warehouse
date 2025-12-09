#!/usr/bin/env python3
"""
Fix SSRS RDL files by adding missing Tablix hierarchy elements.
"""

import os
import re
import sys

def count_columns(rdl_content):
    """Count number of TablixColumn elements in the report."""
    return len(re.findall(r'<TablixColumn>', rdl_content))

def has_hierarchy(rdl_content):
    """Check if report already has hierarchy elements."""
    has_col = '<TablixColumnHierarchy>' in rdl_content
    has_row = '<TablixRowHierarchy>' in rdl_content
    return has_col and has_row

def generate_hierarchy(num_columns):
    """Generate TablixColumnHierarchy and TablixRowHierarchy XML."""
    # Column hierarchy: one TablixMember per column
    col_members = '\n'.join(['                <TablixMember />'] * num_columns)

    hierarchy = f"""            <TablixColumnHierarchy>
              <TablixMembers>
{col_members}
              </TablixMembers>
            </TablixColumnHierarchy>
            <TablixRowHierarchy>
              <TablixMembers>
                <TablixMember>
                  <KeepWithGroup>After</KeepWithGroup>
                </TablixMember>
                <TablixMember>
                  <Group Name="Details" />
                </TablixMember>
              </TablixMembers>
            </TablixRowHierarchy>"""

    return hierarchy

def fix_report(filepath):
    """Fix a single RDL report file."""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Check if already has hierarchy
    if has_hierarchy(content):
        return False, "Already has hierarchy"

    # Count columns
    num_columns = count_columns(content)
    if num_columns == 0:
        return False, "No Tablix found"

    # Generate hierarchy XML
    hierarchy = generate_hierarchy(num_columns)

    # Find insertion point: between </TablixBody> and <DataSetName>
    pattern = r'(</TablixBody>)\s*(<DataSetName>)'

    if not re.search(pattern, content):
        return False, "Could not find insertion point"

    # Insert hierarchy
    new_content = re.sub(
        pattern,
        r'\1\n' + hierarchy + '\n            \2',
        content
    )

    # Write back
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)

    return True, f"Fixed ({num_columns} columns)"

def main():
    reports_dir = '/mnt/c/Projects/rvtools/src/reports'

    # Find all RDL files
    rdl_files = []
    for root, dirs, files in os.walk(reports_dir):
        for file in files:
            if file.endswith('.rdl'):
                rdl_files.append(os.path.join(root, file))

    print(f"Found {len(rdl_files)} RDL files")
    print("-" * 60)

    fixed_count = 0
    skipped_count = 0

    for filepath in sorted(rdl_files):
        filename = os.path.basename(filepath)
        success, message = fix_report(filepath)

        if success:
            print(f"✓ FIXED: {filename:40} {message}")
            fixed_count += 1
        else:
            print(f"  SKIP:  {filename:40} {message}")
            skipped_count += 1

    print("-" * 60)
    print(f"Fixed: {fixed_count}, Skipped: {skipped_count}")

if __name__ == '__main__':
    main()
