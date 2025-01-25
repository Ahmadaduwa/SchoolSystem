function toggleTableDropdown(event, id) {
    const dropdown = document.getElementById(id);
    const rect = event.target.getBoundingClientRect();

    // กำหนดตำแหน่งของ dropdown
    dropdown.style.left = `${rect.left}px`;
    dropdown.style.top = `${rect.bottom}px`;

    // Toggle visibility
    dropdown.classList.toggle('hidden');
    dropdown.classList.toggle('table-dropdown-open');
}