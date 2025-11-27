window.shareQrImage = async (title, text, imageUrl) => {
    try {
        const response = await fetch(imageUrl);
        const blob = await response.blob();
        const file = new File([blob], "pass-gamifyme.png", { type: "image/png" });

        if (navigator.share) {
            await navigator.share({
                title: title,
                text: text,
                files: [file]
            });
        } else {
            alert("Partage non supporté sur cet appareil.");
        }
    } catch (e) {
        console.error("Erreur partage:", e);
        if (navigator.share) {
            await navigator.share({ title, text, url: imageUrl });
        }
    }
};
