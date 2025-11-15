// On garde ça
window.activeScanners = {};

// Fonction utilitaire pour arrêter un scanner en toute sécurité
function stopScanner(scannerId) {
    return new Promise((resolve) => {
        try {
            const html5QrCode = window.activeScanners[scannerId];
            if (html5QrCode && html5QrCode.isScanning) {
                html5QrCode.stop().then(() => {
                    console.log("Scanner QR arrêté.");
                    delete window.activeScanners[scannerId];
                    resolve();
                }).catch(err => {
                    console.error("Échec de l'arrêt du scanner: ", err);
                    delete window.activeScanners[scannerId];
                    resolve();
                });
            } else {
                // Rien à arrêter
                delete window.activeScanners[scannerId];
                resolve();
            }
        } catch (ex) {
            console.error("Erreur à l'arrêt de html5-qrcode: ", ex);
            delete window.activeScanners[scannerId];
            resolve();
        }
    });
}

// Logique de scan (inchangée mais améliorée)
function _runScannerLogic(scannerId, dotnetHelper) {
    try {
        console.log("Html5Qrcode est prêt. Démarrage du scanner...");
        const html5QrCode = new Html5Qrcode(scannerId);
        // flag interne pour éviter doubles notifications
        html5QrCode._hasSignaled = false;
        window.activeScanners[scannerId] = html5QrCode;

        // --- FPS réduit pour diminuer la charge CPU ---
        // Si besoin on peut descendre à 5
        const config = {
            fps: 5,
            qrbox: { width: 250, height: 250 },
            // options expérimentales pour décharger le décodage si la lib/UA le supporte
            experimentalFeatures: {
                useBarCodeDetectorIfSupported: true,
                useWorker: true
            }
        };

        // Callback en cas de succès (async pour attendre l'arrêt propre)
        const onScanSuccess = async (decodedText, decodedResult) => {
            try {
                // Protections : éviter d'appeler plusieurs fois (concurrency)
                const s = window.activeScanners[scannerId];
                if (!s || s._hasSignaled) return;
                s._hasSignaled = true;

                // Arrêter proprement (attendre la fin) avant d'appeler .NET
                await stopScanner(scannerId);

                // Appel .NET une seule fois
                dotnetHelper.invokeMethodAsync('HandleScanResult', decodedText).catch(e => {
                    console.error('Erreur invokeMethodAsync HandleScanResult', e);
                });
            } catch (e) {
                console.error('Erreur onScanSuccess', e);
            }
        };

        const onScanFailure = (error) => { /* ignorer ou logger si besoin */ };

        html5QrCode.start({ facingMode: "environment" }, config, onScanSuccess, onScanFailure);

    } catch (ex) {
        console.error("Erreur (interne) au démarrage de html5-qrcode: ", ex);
    }
}


window.qrScanner = {
    // Logique d'attente (inchangée, elle fonctionne)
    start: (scannerId, dotnetHelper) => {
        if (typeof Html5Qrcode !== 'undefined') {
            _runScannerLogic(scannerId, dotnetHelper);
        } else {
            console.log("Html5Qrcode n'est pas encore prêt, en attente...");
            let attempts = 0;
            const intervalId = setInterval(() => {
                attempts++;
                if (typeof Html5Qrcode !== 'undefined') {
                    clearInterval(intervalId);
                    _runScannerLogic(scannerId, dotnetHelper);
                } else if (attempts > 50) {
                    clearInterval(intervalId);
                    console.error("Échec du chargement de la bibliothèque Html5Qrcode après 5 secondes.");
                }
            }, 100);
        }
    },

    // La fonction 'stop' appelle maintenant notre utilitaire
    stop: (scannerId) => {
        stopScanner(scannerId);
    }
};