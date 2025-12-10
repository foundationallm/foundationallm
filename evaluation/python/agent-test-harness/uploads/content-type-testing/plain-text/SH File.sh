#!/bin/bash

# Process student grades

INPUT_FILE="grades.txt"

# Check if input file exists
if [[ ! -f "$INPUT_FILE" ]]; then
  echo "Error: $INPUT_FILE not found!"
  exit 1
fi

echo "Processing grades from $INPUT_FILE..."
echo ""

total_students=0
total_sum=0

while IFS=',' read -r name grade1 grade2 grade3; do
  if [[ -z "$name" || -z "$grade1" || -z "$grade2" || -z "$grade3" ]]; then
    echo "Skipping invalid line."
    continue
  fi

  sum=$((grade1 + grade2 + grade3))
  avg=$(echo "scale=2; $sum / 3" | bc)

  echo "Student: $name — Grades: $grade1, $grade2, $grade3 — Average: $avg"

  total_students=$((total_students + 1))
  total_sum=$(echo "$total_sum + $avg" | bc)
done < "$INPUT_FILE"

if [[ $total_students -gt 0 ]]; then
  overall_avg=$(echo "scale=2; $total_sum / $total_students" | bc)
  echo ""
  echo "Processed $total_students students. Overall average: $overall_avg"
else
  echo "No valid student data processed."
fi
