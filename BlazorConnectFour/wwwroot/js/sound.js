
// For a sound output
window.playSound = (soundFile) => {
    const audio = new Audio(soundFile);

    audio.play();
};
