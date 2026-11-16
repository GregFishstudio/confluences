function initializePrestations() {
    var width = (window.innerWidth > 0) ? window.innerWidth : document.documentElement.clientWidth;
    if (width > 912) {
        const prestationsSection = document.getElementById('prestations');
        const columns = prestationsSection.querySelectorAll('.col-lg-4');
        const contents = prestationsSection.querySelectorAll('.content_prestations');
        const contentArea = prestationsSection.querySelector('.content-area_prestations');
        const container = prestationsSection.querySelector('.container');
        const section = prestationsSection.querySelector('section.w3-services-ab');

        function showContent2(contentId) {
            contents.forEach(content => {
                if (content.id === contentId) {
                    content.classList.add('active2');
                } else {
                    content.classList.remove('active2');
                }
            });
            columns.forEach(column => {
                const featureGd2 = column.querySelector('.feature-gd2');
                if (featureGd2.getAttribute('data-content') === contentId) {
                    featureGd2.classList.add('active2');
                } else {
                    featureGd2.classList.remove('active2');
                }
            });
            adjustHeight2();
        }

        function adjustHeight2() {
            const activeContent = prestationsSection.querySelector('.content_prestations.active2');
            if (activeContent) {
                const contentHeight = activeContent.offsetHeight;
                contentArea.style.height = `${contentHeight}px`;

                const totalHeight = container.offsetHeight;
                section.style.height = `${totalHeight}px`;
                prestationsSection.style.height = `${totalHeight}px`;
            }
        }

        columns.forEach(column => {
            column.addEventListener('mouseenter', () => {
                const contentId = column.querySelector('.feature-gd2').getAttribute('data-content');
                showContent2(contentId);
            });
        });

        // Show first content by default
        showContent2('content1');

        // Adjust height on window resize
        window.addEventListener('resize', adjustHeight2);
    }
}

function initializeMesures() {
var width = (window.innerWidth > 0) ? window.innerWidth : document.documentElement.clientWidth;
    if(width > 767){
    const mesuresSection = document.getElementById('mesures');
    const columns = mesuresSection.querySelectorAll('.col-md-6');
    const contents = mesuresSection.querySelectorAll('.content_mesures');
    const contentArea = mesuresSection.querySelector('.content-area_mesures');
    const container = mesuresSection.querySelector('.container');
    const section = mesuresSection.querySelector('section.w3-services-ab');

    function showContent(contentId) {
        contents.forEach(content => {
            if (content.id === contentId) {
                content.classList.add('active3');
            } else {
                content.classList.remove('active3');
            }
        }
        );
        columns.forEach(column => {
            const featureGd3 = column.querySelector('.feature-gd3');
            if (featureGd3.getAttribute('data-content') === contentId) {
<!--                column.classList.add('active3');-->
                featureGd3.classList.add('active3');
            } else {
<!--                column.classList.remove('active3');-->
                featureGd3.classList.remove('active3');
            }
        });
        adjustHeight();
    }

    function adjustHeight() {

        const activeContent = mesuresSection.querySelector('.content_mesures.active3');
        if (activeContent) {
            const contentHeight = activeContent.offsetHeight;
            contentArea.style.height = `${contentHeight}px`;

            const totalHeight = container.offsetHeight;
            section.style.height = `${totalHeight}px`;
            mesuresSection.style.height = `${totalHeight}px`;
        }
    }

    columns.forEach(column => {
        column.addEventListener('mouseover', () => {
            const contentId = column.querySelector('.feature-gd3').getAttribute('data-content');
            showContent(contentId);
        });
    });

    // Show first content by default
    showContent('content4');

    // Adjust height on window resize
    window.addEventListener('resize', adjustHeight);

    };
    }
