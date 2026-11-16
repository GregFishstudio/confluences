window.onload = function() {

    // Capture the hash in the URL
    const currentHash = window.location.hash;

    if (currentHash) {
        // Scroll to the target section
        const targetElement = document.querySelector(currentHash);
        if (targetElement) {
            targetElement.scrollIntoView({
                behavior: 'smooth'
            });

            // Update the URL to remove the hash after scrolling
            setTimeout(() => {
                history.replaceState(null, '', window.location.pathname);
            }, 500); // Adjust the delay if needed
        }
    }

    // Add event listeners to all the anchor links in the header
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(event) {
            event.preventDefault(); // Prevent the default link behavior
            const targetId = this.getAttribute('href').substring(1); // Get the target section id
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth'
                });

                // Update the URL in the address bar
                history.replaceState(null, '', window.location.pathname);
            }
        });
    });
};