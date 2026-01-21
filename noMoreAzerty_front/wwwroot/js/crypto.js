// Dérivation de clé (PBKDF2)
window.deriveKeyString = async function (password, salt, iterations) {
    const enc = new TextEncoder();
    const keyMaterial = await crypto.subtle.importKey(
        "raw",
        enc.encode(password + salt),
        "PBKDF2",
        false,
        ["deriveBits"]
    );

    const bits = await crypto.subtle.deriveBits(
        {
            name: "PBKDF2",
            salt: enc.encode(salt),
            iterations: iterations,
            hash: "SHA-256"
        },
        keyMaterial,
        256 // 256 bits = 32 bytes
    );

    return btoa(String.fromCharCode(...new Uint8Array(bits)));
};

// Génération d'une clé aléatoire
window.generateRandomKey = function (length) {
    const array = new Uint8Array(length);
    crypto.getRandomValues(array);
    return btoa(String.fromCharCode(...array));
};

// Génération d'un IV aléatoire
window.generateRandomBytes = function (length) {
    const array = new Uint8Array(length);
    crypto.getRandomValues(array);
    return btoa(String.fromCharCode(...array));
};