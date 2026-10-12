// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-hero-carousel]').forEach(initializeHeroCarousel);
});

function initializeHeroCarousel(carousel) {
    const slides = Array.from(carousel.querySelectorAll('[data-carousel-slide]'));
    const previousButton = carousel.querySelector('[data-carousel-prev]');
    const nextButton = carousel.querySelector('[data-carousel-next]');
    const indicators = Array.from(carousel.querySelectorAll('[data-carousel-indicator]'));
    const reducedMotionQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    const interval = Number.parseInt(carousel.dataset.interval ?? '3500', 10);

    if (slides.length === 0) {
        return;
    }

    let activeIndex = Math.max(slides.findIndex(slide => slide.classList.contains('is-active')), 0);
    let timerId = null;
    let pointerPaused = false;
    let focusPaused = false;

    const isPaused = () => pointerPaused || focusPaused || reducedMotionQuery.matches || slides.length < 2;

    const setActiveSlide = (index) => {
        activeIndex = (index + slides.length) % slides.length;

        slides.forEach((slide, slideIndex) => {
            const isActive = slideIndex === activeIndex;
            slide.classList.toggle('is-active', isActive);
            slide.setAttribute('aria-hidden', String(!isActive));
        });

        indicators.forEach((indicator, indicatorIndex) => {
            const isActive = indicatorIndex === activeIndex;
            indicator.classList.toggle('is-active', isActive);
            indicator.setAttribute('aria-current', String(isActive));
        });
    };

    const stopAutoplay = () => {
        if (timerId !== null) {
            window.clearInterval(timerId);
            timerId = null;
        }
    };

    const startAutoplay = () => {
        stopAutoplay();

        if (isPaused()) {
            return;
        }

        timerId = window.setInterval(() => {
            setActiveSlide(activeIndex + 1);
        }, interval);
    };

    const goToSlide = (index) => {
        setActiveSlide(index);
        startAutoplay();
    };

    previousButton?.addEventListener('click', () => {
        goToSlide(activeIndex - 1);
    });

    nextButton?.addEventListener('click', () => {
        goToSlide(activeIndex + 1);
    });

    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => {
            goToSlide(index);
        });
    });

    carousel.addEventListener('mouseenter', () => {
        pointerPaused = true;
        stopAutoplay();
    });

    carousel.addEventListener('mouseleave', () => {
        pointerPaused = false;
        startAutoplay();
    });

    carousel.addEventListener('focusin', () => {
        focusPaused = true;
        stopAutoplay();
    });

    carousel.addEventListener('focusout', (event) => {
        if (event.relatedTarget && carousel.contains(event.relatedTarget)) {
            return;
        }

        focusPaused = false;
        startAutoplay();
    });

    const reducedMotionHandler = () => {
        if (reducedMotionQuery.matches) {
            stopAutoplay();
            return;
        }

        startAutoplay();
    };

    if (typeof reducedMotionQuery.addEventListener === 'function') {
        reducedMotionQuery.addEventListener('change', reducedMotionHandler);
    } else if (typeof reducedMotionQuery.addListener === 'function') {
        reducedMotionQuery.addListener(reducedMotionHandler);
    }

    setActiveSlide(activeIndex);
    startAutoplay();
}
