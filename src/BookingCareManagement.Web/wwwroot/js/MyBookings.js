// Dropdown functionality
const filterDropdown = document.getElementById('filterDropdown');
const dropdownMenu = document.getElementById('dropdownMenu');
const dropdownItems = document.querySelectorAll('.dropdown-item-custom');

// Toggle dropdown
filterDropdown.addEventListener('click', function (e) {
    e.stopPropagation();
    dropdownMenu.classList.toggle('show');
    filterDropdown.classList.toggle('active');
});

// Select dropdown item
dropdownItems.forEach(item => {
    item.addEventListener('click', function () {
        // Remove active class from all items
        dropdownItems.forEach(i => i.classList.remove('active'));

        // Add active class to clicked item
        this.classList.add('active');

        // Update dropdown button text
        const selectedText = this.textContent;
        filterDropdown.querySelector('span').textContent = selectedText;

        // Close dropdown
        dropdownMenu.classList.remove('show');
        filterDropdown.classList.remove('active');

        // Log selected filter
        console.log('Selected filter:', selectedText);
    });
});

// Close dropdown when clicking outside
document.addEventListener('click', function (e) {
    if (!filterDropdown.contains(e.target) && !dropdownMenu.contains(e.target)) {
        dropdownMenu.classList.remove('show');
        filterDropdown.classList.remove('active');
    }
});

// Book Now button handlers
const bookNowButtons = document.querySelectorAll('.btn-book-now, .btn-book-now-secondary');

bookNowButtons.forEach(button => {
    button.addEventListener('click', function () {
        console.log('Book Now clicked');
        alert('Redirecting to booking page...');
        // In a real app, this would navigate to the booking page
        // window.location.href = '/book';
    });
});