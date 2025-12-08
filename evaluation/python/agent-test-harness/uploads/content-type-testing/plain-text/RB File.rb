require 'json'

class Student
  attr_reader :name, :grades

  def initialize(name, grades)
    @name = name
    @grades = grades
  end

  def average
    grades.sum.to_f / grades.size
  end
end

# Create student objects
students = [
  Student.new("Alice", [85, 90, 88]),
  Student.new("Bob", [70, 75, 80]),
  Student.new("Charlie", [95, 92, 96])
]

# Save to JSON file
data = students.map { |student| { name: student.name, grades: student.grades } }
File.write("students.json", JSON.pretty_generate(data))

puts "Saved students to students.json"

# Read JSON and calculate averages
loaded_data = JSON.parse(File.read("students.json"))

loaded_data.each do |student_data|
  name = student_data["name"]
  grades = student_data["grades"]
  avg = grades.sum.to_f / grades.size
  puts "#{name}'s average grade: #{avg.round(2)}"
end
