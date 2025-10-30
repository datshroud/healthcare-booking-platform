// Star rating functionality
const stars = document.querySelectorAll('.star');
let selectedRating = 5;

// Click event for stars
stars.forEach(star => {
    star.addEventListener('click', function () {
        selectedRating = parseInt(this.getAttribute('data-rating'));
        updateStars();
    });

    // Hover effect
    star.addEventListener('mouseenter', function () {
        const rating = parseInt(this.getAttribute('data-rating'));
        stars.forEach((s, index) => {
            if (index < rating) {
                s.classList.remove('inactive');
            } else {
                s.classList.add('inactive');
            }
        });
    });
});

// Reset to selected rating when mouse leaves
document.querySelector('.stars-container').addEventListener('mouseleave', updateStars);

// Update stars display based on selected rating
function updateStars() {
    stars.forEach((star, index) => {
        if (index < selectedRating) {
            star.classList.remove('inactive');
        } else {
            star.classList.add('inactive');
        }
    });
}

// Initialize with 5 stars selected
updateStars();

// Form submission handler (optional)
document.querySelector('form').addEventListener('submit', function (e) {
    e.preventDefault();

    const textarea = this.querySelector('textarea').value;
    const privacy = this.querySelector('input[name="privacy"]:checked').id;

    console.log('Rating:', selectedRating);
    console.log('Review:', textarea);
    console.log('Privacy:', privacy);

    alert('Review submitted! Rating: ' + selectedRating + ' stars');
});