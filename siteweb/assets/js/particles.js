function particlesForWidth(width) {
  return Math.max(15, Math.floor(width / 40));
}

function linkDistance(width) {
//console.log(width);
    return Math.max(50, Math.floor(width / 2));
}


tsParticles.load("tsparticles", {
  fullScreen: { enable: false },
  background: { color: "transparent" },
  fpsLimit: 60,

  particles: {
    color: { value: "#fff" },
    size: { value: { min: 2, max: 3 } },
    opacity: { value: 0.3 },

    move: {
      enable: true,
      speed: {
        min: 0.2,
        max: 1.5
          },
//      direction: "down",
      random: true,
      straight: false,
      outModes: { default: "bounce" }
    },

    links: {
      enable: true,
      distance: 400,
      frequency: 0.25,
      opacity: 0.3
    }
  },

  emitters: {
    position: { x: 50, y: 100, mode: "percent" },

    rate: {
      quantity: 1,
      delay: 3
    },

    life: {
      duration: 1,   // placeholder, overwritten
      count: 1
    }
  }

}).then(container => {
  const width = container.canvas.element.clientWidth;
  const total = particlesForWidth(width);
  const distance = linkDistance(width);

//  console.log(distance);

  container.options.particles.links.distance = distance;

//  console.log("Target particle count:", total);

  const emitter = container.getEmitter();
  if (!emitter) return;

  emitter.options.life.duration = total * emitter.options.rate.delay;

  emitter.play();

//  const log = setInterval(() => {
//    console.log("Current particles:", container.particles.count);
//
//    if (container.particles.count >= total) {
//      console.log("Emission complete");
//      clearInterval(log);
//    }
//  }, 1000);
});
