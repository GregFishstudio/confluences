// Testimonial Carousel
let currentSlideIndex = 0;
let autoplayInterval;
let isAutoplayRunning = true;
let isAnimating = false;

function showSlide(index, direction = 1) {
    if (isAnimating) return;
    
    const slides = document.querySelectorAll('.testimonial-slide');
    const dots = document.querySelectorAll('.carousel-dots .dot');
    
    if (!slides.length) return;
    
    let newIndex;
    if (index >= slides.length) {
        newIndex = 0;
    } else if (index < 0) {
        newIndex = slides.length - 1;
    } else {
        newIndex = index;
    }
    
    if (newIndex === currentSlideIndex) return;
    
    const currentSlide = slides[currentSlideIndex];
    const nextSlide = slides[newIndex];
    
    isAnimating = true;
    
    slides.forEach(slide => {
        slide.classList.remove('active', 'slide-in-right', 'slide-out-left', 'slide-in-left', 'slide-out-right');
    });
    
    if (direction > 0) {
        currentSlide.classList.add('slide-out-left');
        nextSlide.classList.add('slide-in-right', 'active');
    } else {
        currentSlide.classList.add('slide-out-right');
        nextSlide.classList.add('slide-in-left', 'active');
    }
    
    currentSlideIndex = newIndex;
    
    dots.forEach((dot, i) => {
        if (i === currentSlideIndex) {
            dot.classList.add('active');
        } else {
            dot.classList.remove('active');
        }
    });
    
    setTimeout(() => {
        isAnimating = false;
        currentSlide.classList.remove('slide-out-left', 'slide-out-right');
        currentSlide.classList.remove('active');
    }, 600);
}

function changeSlide(direction) {
    showSlide(currentSlideIndex + direction, direction);
    resetAutoplay();
}

function goToSlide(index) {
    const direction = index > currentSlideIndex ? 1 : -1;
    showSlide(index, direction);
    resetAutoplay();
}

function autoplaySlides() {
    autoplayInterval = setInterval(() => {
        showSlide(currentSlideIndex + 1, 1);
    }, 15000);
}

function toggleAutoplay() {
    const pauseIcon = document.querySelector('.carousel-playpause .pause-icon');
    const playIcon = document.querySelector('.carousel-playpause .play-icon');
    
    if (isAutoplayRunning) {
        clearInterval(autoplayInterval);
        pauseIcon.style.display = 'none';
        playIcon.style.display = 'inline-block';
        playIcon.style.fontSize = '20px';
        isAutoplayRunning = false;
    } else {
        autoplaySlides();
        pauseIcon.style.display = 'inline-block';
        pauseIcon.style.marginTop = '3px';
        playIcon.style.display = 'none';
        isAutoplayRunning = true;
    }
}

function resetAutoplay() {
    if (isAutoplayRunning) {
        clearInterval(autoplayInterval);
        autoplaySlides();
    }
}

// Initialize carousel
document.addEventListener('DOMContentLoaded', function() {
    const slides = document.querySelectorAll('.testimonial-slide');
    if (slides.length > 0) {
        slides[0].classList.add('active');
    }
    autoplaySlides();
});

// Pause autoplay when user leaves the page
document.addEventListener('visibilitychange', function() {
    if (document.hidden && isAutoplayRunning) {
        clearInterval(autoplayInterval);
    } else if (!document.hidden && isAutoplayRunning) {
        autoplaySlides();
    }
});

// Prevent carousel from blocking page scroll on mobile
document.addEventListener('DOMContentLoaded', function() {
    const carousel = document.querySelector('.testimonial-carousel');
    if (carousel) {
        let touchStartY = 0;
        
        carousel.addEventListener('touchstart', function(e) {
            touchStartY = e.touches[0].clientY;
        }, {passive: true});
        
        carousel.addEventListener('touchmove', function(e) {
            const touchY = e.touches[0].clientY;
            const touchDiff = touchStartY - touchY;
            
            // Only prevent default if trying to scroll horizontally
            // Allow vertical scrolling
            if (Math.abs(touchDiff) < 10) {
                // This is likely horizontal swipe, let carousel handle it
            } else {
                // This is vertical scroll, don't interfere
            }
        }, {passive: true});
    }
});
