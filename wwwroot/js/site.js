// Healthcare App JavaScript

// Initialize when document is ready
$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Smooth scrolling for anchor links
    $('a[href*="#"]').not('[href="#"]').not('[href="#0"]').click(function(event) {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                event.preventDefault();
                $('html, body').animate({
                    scrollTop: target.offset().top - 100
                }, 1000);
            }
        }
    });

    // Auto-hide alerts after 5 seconds
    $('.alert').each(function() {
        var alert = $(this);
        setTimeout(function() {
            alert.fadeOut('slow');
        }, 5000);
    });

    // Form validation enhancement
    $('.needs-validation').on('submit', function(event) {
        if (this.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        }
        $(this).addClass('was-validated');
    });

    // Loading states for buttons
    $('.btn-loading').on('click', function() {
        var btn = $(this);
        var originalText = btn.html();
        btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
        btn.prop('disabled', true);
        
        // Re-enable after 3 seconds (adjust as needed)
        setTimeout(function() {
            btn.html(originalText);
            btn.prop('disabled', false);
        }, 3000);
    });

    // Sidebar toggle for mobile
    $('.sidebar-toggle').on('click', function() {
        $('.sidebar').toggleClass('show');
        $('.content').toggleClass('sidebar-open');
    });

    // Close sidebar when clicking outside on mobile
    $(document).on('click', function(event) {
        if ($(window).width() <= 768) {
            if (!$(event.target).closest('.sidebar, .sidebar-toggle').length) {
                $('.sidebar').removeClass('show');
                $('.content').removeClass('sidebar-open');
            }
        }
    });

    // Animate cards on scroll
    function animateOnScroll() {
        $('.card').each(function() {
            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).outerHeight();
            var viewportTop = $(window).scrollTop();
            var viewportBottom = viewportTop + $(window).height();

            if (elementBottom > viewportTop && elementTop < viewportBottom) {
                $(this).addClass('fade-in');
            }
        });
    }

    // Run animation on scroll
    $(window).on('scroll', animateOnScroll);
    animateOnScroll(); // Run once on load

    // Enhanced form interactions
    $('.form-control, .form-select').on('focus', function() {
        $(this).parent().addClass('focused');
    }).on('blur', function() {
        $(this).parent().removeClass('focused');
    });

    // Real-time search functionality (if needed)
    $('.search-input').on('input', function() {
        var searchTerm = $(this).val().toLowerCase();
        var searchTarget = $(this).data('target');
        
        $(searchTarget + ' .searchable-item').each(function() {
            var text = $(this).text().toLowerCase();
            if (text.includes(searchTerm)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // Confirmation dialogs
    $('.confirm-action').on('click', function(event) {
        var message = $(this).data('confirm') || 'Are you sure you want to perform this action?';
        if (!confirm(message)) {
            event.preventDefault();
            return false;
        }
    });

    // Auto-refresh for dashboard (every 5 minutes)
    if (window.location.pathname.includes('Dashboard')) {
        setInterval(function() {
            // Only refresh if the page is visible
            if (!document.hidden) {
                location.reload();
            }
        }, 300000); // 5 minutes
    }
});

// Utility functions
function showNotification(message, type = 'info') {
    var alertClass = 'alert-' + type;
    var alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('.main-content').prepend(alertHtml);
    
    // Auto-hide after 5 seconds
    setTimeout(function() {
        $('.alert').first().fadeOut('slow');
    }, 5000);
}

function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatTime(timeString) {
    var time = new Date('1970-01-01T' + timeString);
    return time.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });
}

// Export functions for global use
window.HealthcareApp = {
    showNotification: showNotification,
    formatDate: formatDate,
    formatTime: formatTime
};