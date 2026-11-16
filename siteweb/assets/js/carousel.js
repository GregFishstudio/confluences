const container = document.getElementById('container');
const sentences = [
    "Des liens qui se tissent",
    "Des chemins qui se dessinent",
    "Un avenir qui s'esquisse"
];

function createSentence(text, index) {
    const sentence = document.createElement('div');
    sentence.className = 'sentence';
    sentence.textContent = text;
    sentence.style.position = 'absolute';
    sentence.style.top = `${container.clientHeight}px`;
    container.appendChild(sentence);

    setTimeout(() => {
        // Get the total height of all previous sentences combined
        let previousHeight = 0;
        for (let i = 0; i < index; i++) {
            previousHeight += container.children[i].offsetHeight;
        }

        // Set the top position based on the height of the previous sentences
        sentence.style.top = `${previousHeight+10}px`;
    }, 100);

    return sentence;
}

function animateSentences() {
    container.innerHTML = '';
    let delay = 0;

    sentences.forEach((text, index) => {
        setTimeout(() => {
            createSentence(text, index);
        }, delay);
        delay += 5000;
    });

//    setTimeout(() => {
//        container.innerHTML = '';
//        setTimeout(animateSentences, 2000);
//    }, delay + 3000);
}

animateSentences();
