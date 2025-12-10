import json
from statistics import mean

class Student:
    def __init__(self, name, grades):
        self.name = name
        self.grades = grades
    
    def average(self):
        return mean(self.grades)

# Save student data to JSON
students = [
    Student("Alice", [85, 90, 88]),
    Student("Bob", [70, 75, 80]),
    Student("Charlie", [95, 92, 96])
]

data = {student.name: student.grades for student in students}

with open("students.json", "w") as f:
    json.dump(data, f, indent=4)

# Read JSON back and compute averages
with open("students.json", "r") as f:
    loaded_data = json.load(f)

for name, grades in loaded_data.items():
    avg = mean(grades)
    print(f"{name}'s average grade: {avg:.2f}")
