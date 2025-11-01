// Animation on page load
document.addEventListener('DOMContentLoaded', function () {
    // Fade in the success card
    const successCard = document.querySelector('.success-card');
    successCard.style.opacity = '0';
    successCard.style.transform = 'translateY(20px)';

    setTimeout(() => {
        successCard.style.transition = 'all 0.6s ease-out';
        successCard.style.opacity = '1';
        successCard.style.transform = 'translateY(0)';
    }, 100);

    // Add confetti effect (optional)
    createConfetti();
});

// Optional: Redirect to home or bookings page after a few seconds
setTimeout(() => {
    console.log('Success page displayed for 5 seconds');
    // Uncomment to redirect
    // window.location.href = '/bookings';
}, 5000);

// Simple confetti effect
function createConfetti() {
    const colors = ['#48bb78', '#4299e1', '#fbbf24'];
    const confettiCount = 30;

    for (let i = 0; i < confettiCount; i++) {
        setTimeout(() => {
            const confetti = document.createElement('div');
            confetti.style.position = 'fixed';
            confetti.style.width = '10px';
            confetti.style.height = '10px';
            confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
            confetti.style.left = Math.random() * 100 + '%';
            confetti.style.top = '-10px';
            confetti.style.opacity = '0.8';
            confetti.style.borderRadius = '50%';
            confetti.style.pointerEvents = 'none';
            confetti.style.zIndex = '1000';

            document.body.appendChild(confetti);

            // Animate falling
            let top = -10;
            let left = parseFloat(confetti.style.left);
            const speed = 2 + Math.random() * 3;
            const sway = (Math.random() - 0.5) * 2;

            const fall = setInterval(() => {
                top += speed;
                left += sway * 0.5;
                confetti.style.top = top + 'px';
                confetti.style.left = left + '%';
                confetti.style.opacity = parseFloat(confetti.style.opacity) - 0.01;

                if (top > window.innerHeight || parseFloat(confetti.style.opacity) <= 0) {
                    clearInterval(fall);
                    confetti.remove();
                }
            }, 20);
        }, i * 50);
    }
}

// Log success submission
console.log('Review submitted successfully!');
console.log('Service: Therapeutic Exercise');
console.log('Employee: Jane Doe');
console.log('Date: 30 October 2025 12:30');