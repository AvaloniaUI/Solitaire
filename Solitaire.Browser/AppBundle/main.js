const loader = document.getElementById('solitaire-loader');
const host = document.getElementById('out');
const status = document.getElementById('loading-status');
const progress = document.getElementById('loading-progress');
const fill = document.getElementById('loading-fill');
const retry = document.getElementById('loading-retry');
let loaded = 0;

function updateProgress(value) {
    loaded = Math.max(loaded, Math.min(100, value));
    progress.setAttribute('aria-valuenow', String(Math.round(loaded)));
    fill.style.transform = `scaleX(${loaded / 100})`;
}

// Convert the TitleView bounds to CSS pixels.
let revealing = false;
let releaseRendering;
const renderingReady = new Promise(resolve => { releaseRendering = resolve; });
globalThis.solitaireLoader = {
    ready() {
        updateProgress(100);
        status.textContent = 'Ready';
        releaseRendering();
    },
    begin(x, y, width, height) {
        if (revealing) return;
        revealing = true;
        const animations = [];
        let finished = false;
        const finish = async () => {
            // Keep the cover and input lock until the prepared frame is ready.
            await renderingReady;
            if (finished) return;
            finished = true;
            removeEventListener('resize', finish);
            loader.remove();
            host.inert = false;
            for (const animation of animations) animation.cancel();
        };
        if (matchMedia('(prefers-reduced-motion: reduce)').matches || width <= 0 || height <= 0) {
            finish();
            return;
        }
        const viewport = host.getBoundingClientRect();
        x = viewport.x + x * viewport.width;
        y = viewport.y + y * viewport.height;
        width *= viewport.width;
        height *= viewport.height;
        // After a resize, skip the animation and wait for the prepared frame.
        addEventListener('resize', finish, { once: true });
        const artwork = loader.querySelector('.loader-artwork');
        const start = artwork.getBoundingClientRect();
        const dx = x + width / 2 - (start.x + start.width / 2);
        const dy = y + height / 2 - (start.y + start.height / 2);
        const move = artwork.animate([
            { transform: 'translate(-50%, -50%)' },
            { transform: `translate(calc(-50% + ${dx}px), calc(-50% + ${dy}px)) scale(${width / start.width})` }
        ], { duration: 500, easing: 'cubic-bezier(.4, 0, .2, 1)', fill: 'forwards' });
        animations.push(move);
        renderingReady.then(() => {
            if (!finished) animations.push(loader.querySelector('.loader-status').animate(
                [{ opacity: 1 }, { opacity: 0 }], { duration: 180, fill: 'forwards' }));
        });
        // The cover hides the menu logo during the move.
        Promise.all([move.finished, renderingReady]).then(() => {
            if (finished) return;
            const reveal = loader.animate([{ opacity: 1 }, { opacity: 0 }],
                { duration: 220, easing: 'ease-in-out', fill: 'forwards' });
            animations.push(reveal);
            return reveal.finished;
        }).then(finish, finish);
    }
};

retry.addEventListener('click', () => location.reload());

try {
    const { dotnet } = await import('./_framework/dotnet.js');
    const dotnetRuntime = await dotnet
        .withDiagnosticTracing(false)
        // Content hashes let the browser reuse cached runtime files.
        .withConfig({ disableNoCacheFetch: true, maxParallelDownloads: 64 })
        .withApplicationArgumentsFromQuery()
        .withModuleConfig({
            onDownloadResourceProgress(current, total) {
                if (total > 0) updateProgress(90 * current / total);
            }
        })
        .create();

    updateProgress(90);
    status.textContent = 'Preparing the table…';
    const config = dotnetRuntime.getConfig();
    await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
} catch (error) {
    status.textContent = 'Solitaire could not load. Please try again.';
    progress.hidden = true;
    retry.hidden = false;
    console.error(error);
}
