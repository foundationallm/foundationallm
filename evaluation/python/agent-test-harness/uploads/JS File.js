// Select DOM elements
const input = document.getElementById("taskInput");
const addBtn = document.getElementById("addBtn");
const taskList = document.getElementById("taskList");

// Add event listener to button
addBtn.addEventListener("click", () => {
  const taskText = input.value.trim();

  if (taskText === "") {
    alert("Please enter a task!");
    return;
  }

  // Create a new list item
  const li = document.createElement("li");
  li.textContent = taskText;

  // Add a remove button
  const removeBtn = document.createElement("button");
  removeBtn.textContent = "❌";
  removeBtn.style.marginLeft = "10px";

  // Event to remove task
  removeBtn.addEventListener("click", () => {
    li.remove();
  });

  // Append button to li, and li to the list
  li.appendChild(removeBtn);
  taskList.appendChild(li);

  // Clear input field
  input.value = "";
});
