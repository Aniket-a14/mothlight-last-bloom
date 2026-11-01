# Mothlight: Last Bloom — Mini GDD

- **Genre / concept:** short-run, top-down arcade game about a moth tending a moonflower in a dangerous night garden.
- **Player:** a tiny lantern moth, moved with WASD or arrow keys.
- **Distinctive mechanic:** hold Space or left mouse to hum, pulling nearby drifting fireflies into an orbiting bundle (maximum six); release to burst the bundle outward. Near the moonflower, the burst delivers its light. Elsewhere, it shoves thorn spirits away.
- **Loop:** sweep through the garden → gather a few lights → return toward the flower while avoiding spirits → release a pulse to bank progress or clear room → repeat as the garden grows busier.
- **Objective / end:** fill three petals before 180 seconds expire. Three hits or the timer ending loses the run. Retry starts with clean state; title, pause, win, and loss screens are included.
- **Progression:** fireflies respawn; spirit pressure gradually rises and gains another pursuer at one minute and two minutes. A delivered bundle visibly advances the flower.
- **Visual identity:** original hand-drawn-in-code storybook sprites, soft pixel-like edges, twilight indigo garden, warm amber fireflies, pale lilac moth and coral thorn spirits. Palette: night `#11162E`, deep blue `#252C52`, fern `#315C58`, moon `#E9D9AA`, moth `#E9C5D1`, pollen `#FFD66D`, spirit `#FF7B83`.
- **Audio:** synthesized mellow night drone, airy gather chimes, round pulse note, damage thump, and a three-note bloom fanfare.
- **Minimum complete scope:** one bounded garden, one player, gather/release pulse, 21 lights to bloom three petals, three lives, escalating pursuers, timer, menu/pause/end/retry, keyboard and mouse support, local macOS app.
