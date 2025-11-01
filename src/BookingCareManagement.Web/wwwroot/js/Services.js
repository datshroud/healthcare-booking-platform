// Dropdown functionality
const employeeFilter = document.getElementById('employeeFilter');
const employeeMenu = document.getElementById('employeeMenu');
const sortFilter = document.getElementById('sortFilter');
const sortMenu = document.getElementById('sortMenu');
const searchInput = document.getElementById('searchInput');

// Toggle Employee dropdown
employeeFilter.addEventListener('click', function (e) {
    e.stopPropagation();
    employeeMenu.classList.toggle('show');
    employeeFilter.classList.toggle('active');

    // Close sort menu if open
    sortMenu.classList.remove('show');
    sortFilter.classList.remove('active');
});

// Toggle Sort dropdown
sortFilter.addEventListener('click', function (e) {
    e.stopPropagation();
    sortMenu.classList.toggle('show');
    sortFilter.classList.toggle('active');

    // Close employee menu if open
    employeeMenu.classList.remove('show');
    employeeFilter.classList.remove('active');
});

// Handle employee filter selection
document.querySelectorAll('#employeeMenu .dropdown-item-custom').forEach(item => {
    item.addEventListener('click', function () {
        const selectedText = this.textContent;
        employeeFilter.querySelector('span').textContent = selectedText;
        employeeMenu.classList.remove('show');
        employeeFilter.classList.remove('active');
        console.log('Employee filter:', selectedText);
        filterServices();
    });
});

// Handle sort selection
document.querySelectorAll('#sortMenu .dropdown-item-custom').forEach(item => {
    item.addEventListener('click', function () {
        const selectedText = this.textContent;
        sortFilter.querySelector('span').textContent = selectedText;
        sortMenu.classList.remove('show');
        sortFilter.classList.remove('active');
        console.log('Sort by:', selectedText);
        sortServices(selectedText);
    });
});

// Close dropdowns when clicking outside
document.addEventListener('click', function (e) {
    if (!employeeFilter.contains(e.target) && !employeeMenu.contains(e.target)) {
        employeeMenu.classList.remove('show');
        employeeFilter.classList.remove('active');
    }
    if (!sortFilter.contains(e.target) && !sortMenu.contains(e.target)) {
        sortMenu.classList.remove('show');
        sortFilter.classList.remove('active');
    }
});

// Search functionality
searchInput.addEventListener('input', function () {
    const searchTerm = this.value.toLowerCase();
    const serviceCards = document.querySelectorAll('.service-card');

    serviceCards.forEach(card => {
        const serviceName = card.querySelector('.service-name').textContent.toLowerCase();
        if (serviceName.includes(searchTerm)) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });

    console.log('Search:', searchTerm);
});

// Filter services (placeholder function)
function filterServices() {
    console.log('Filtering services...');
    // Implementation would filter by employee
}

// Sort services
function sortServices(sortType) {
    const grid = document.querySelector('.services-grid');
    const cards = Array.from(document.querySelectorAll('.service-card'));

    cards.sort((a, b) => {
        switch (sortType) {
            case 'Price (lowest)':
                const priceA = parseFloat(a.querySelector('.service-price').textContent.replace('$', ''));
                const priceB = parseFloat(b.querySelector('.service-price').textContent.replace('$', ''));
                return priceA - priceB;

            case 'Price (highest)':
                const priceA2 = parseFloat(a.querySelector('.service-price').textContent.replace('$', ''));
                const priceB2 = parseFloat(b.querySelector('.service-price').textContent.replace('$', ''));
                return priceB2 - priceA2;

            case 'Name (ascending)':
                const nameA = a.querySelector('.service-name').textContent;
                const nameB = b.querySelector('.service-name').textContent;
                return nameA.localeCompare(nameB);

            case 'Name (descending)':
                const nameA2 = a.querySelector('.service-name').textContent;
                const nameB2 = b.querySelector('.service-name').textContent;
                return nameB2.localeCompare(nameA2);

            default:
                return 0;
        }
    });

    // Re-append sorted cards
    cards.forEach(card => grid.appendChild(card));
}

// Book Now button handlers
document.querySelectorAll('.btn-primary').forEach(button => {
    button.addEventListener('click', function () {
        const serviceCard = this.closest('.service-card');
        const serviceName = serviceCard.querySelector('.service-name').textContent;
        console.log('Booking:', serviceName);
        alert(`Booking service: ${serviceName}`);
    });
});

// Learn More button handlers
document.querySelectorAll('.btn-outline').forEach(button => {
    button.addEventListener('click', function () {
        const serviceCard = this.closest('.service-card');
        const serviceName = serviceCard.querySelector('.service-name').textContent;
        console.log('Learn more about:', serviceName);
        alert(`Learn more about: ${serviceName}`);
    });
});

// Fade in animation on load
document.addEventListener('DOMContentLoaded', function () {
    const cards = document.querySelectorAll('.service-card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease-out';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });
});