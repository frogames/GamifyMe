function downloadFile(filename, dataUrl) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.generateAndDownloadQrCode = async (text, filename) => {
    try {
        const apiUrl = `https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=${encodeURIComponent(text)}`;
        const response = await fetch(apiUrl);
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        
        downloadFile(filename, url);
        
        window.URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Error generating QR code:', error);
        alert('Erreur lors de la génération du QR Code.');
    }
};
