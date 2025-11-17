$(document).ready(function () {
    // Specialty modal function
    window.showSpecialtyInfo = function (title, doctors, description) {
        $('#specialtyTitle').text(title);
        $('#specialtyDoctors').text(doctors);
        $('#specialtyDescription').text(description);
    };

    // Doctor modal function
    window.showDoctorInfo = function (name, specialty, experience, reviews, description) {
        $('#doctorName').text(name);
        $('#doctorSpecialty').text('Chuyên khoa: ' + specialty);
        $('#doctorExperience').text(experience + ' kinh nghiệm');
        $('#doctorReviews').text('(' + reviews + ' đánh giá)');
        $('#doctorDescription').text(description);
    };

    // Image Slider for Hero Section
    let currentSlide = 0;
    const slides = $('.image-slide');
    const totalSlides = slides.length;

    function showSlide(index) {
        slides.removeClass('active');
        $(slides[index]).addClass('active');
    }

    function nextSlide() {
        currentSlide = (currentSlide + 1) % totalSlides;
        showSlide(currentSlide);
    }

    // Auto slide every 5 seconds
    setInterval(nextSlide, 5000);

    // Navbar scroll effect
    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('.navbar').addClass('scrolled');
            $('#scrollTop').fadeIn();
        } else {
            $('.navbar').removeClass('scrolled');
            $('#scrollTop').fadeOut();
        }
    });

    // Scroll to top
    $('#scrollTop').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
    });

    // Smooth scrolling
    $('a[href^="#"]').on('click', function (e) {
        e.preventDefault();
        var target = $(this.hash);
        if (target.length) {
            $('html, body').animate({
                scrollTop: target.offset().top - 70
            }, 800);
        }
    });

    // Counter animation
    function animateCounter() {
        $('.counter').each(function () {
            var $this = $(this);
            var countTo = $this.attr('data-target');
            $({ countNum: 0 }).animate({
                countNum: countTo
            }, {
                duration: 2000,
                easing: 'swing',
                step: function () {
                    $this.text(Math.floor(this.countNum).toLocaleString());
                },
                complete: function () {
                    $this.text(this.countNum.toLocaleString());
                }
            });
        });
    }

    // Scroll reveal animation
    function revealOnScroll() {
        $('.reveal').each(function () {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('active');
            }
        });
    }

    // Check if stats section is in view for counter
    var statsAnimated = false;
    function checkStats() {
        var statsSection = $('.stats-section');
        if (statsSection.length) {
            var elementTop = statsSection.offset().top;
            var elementBottom = elementTop + statsSection.outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (!statsAnimated && elementBottom > viewportTop && elementTop < viewportBottom) {
                animateCounter();
                statsAnimated = true;
            }
        }
    }

    $(window).on('scroll', function () {
        revealOnScroll();
        checkStats();
    });

    // Initial check
    revealOnScroll();
    checkStats();

    // Add pulse animation to CTA buttons
    setInterval(function () {
        $('.btn-hero-primary').addClass('pulse');
        setTimeout(function () {
            $('.btn-hero-primary').removeClass('pulse');
        }, 1000);
    }, 5000);

    // Floating buttons hover effect
    $('.floating-btn').hover(
        function () {
            $(this).css('transform', 'scale(1.1) rotate(5deg)');
        },
        function () {
            $(this).css('transform', 'scale(1) rotate(0deg)');
        }
    );

    // Feature cards entrance animation
    $('.feature-card').each(function (index) {
        $(this).css('animation-delay', (index * 0.1) + 's');
    });

    // Specialty cards entrance animation
    $('.specialty-card').each(function (index) {
        $(this).css('animation-delay', (index * 0.05) + 's');
    });
});