async function encryptAesGcm(password, salt, data, iterations = 100000) {
    const iv = window.crypto.getRandomValues(new Uint8Array(12));
    const key = await window.deriveKey(password, Uint8Array.from(atob(salt), c => c.charCodeAt(0)), iterations);

    const encoder = new TextEncoder();
    const encrypted = await window.crypto.subtle.encrypt(
        { name: "AES-GCM", iv: iv },
        key,
        encoder.encode(data)
    );
    const encryptedArray = new Uint8Array(encrypted);

    // Tag = 16 derniers octets
    const tag = encryptedArray.slice(-16);
    const ciphertext = encryptedArray.slice(0, -16);

    return {
        salt: salt,
        iv: btoa(String.fromCharCode(...iv)),
        ciphertext: btoa(String.fromCharCode(...ciphertext)),
        tag: btoa(String.fromCharCode(...tag))
    };
}
window.encryptAesGcm = encryptAesGcm;

async function decryptAesGcm(password, salt, iv, tag, ciphertext, iterations = 100000) {
    const ivBytes = Uint8Array.from(atob(iv), c => c.charCodeAt(0));
    const tagBytes = Uint8Array.from(atob(tag), c => c.charCodeAt(0));
    const ciphertextBytes = Uint8Array.from(atob(ciphertext), c => c.charCodeAt(0));

    // Recompose le buffer chiffré + tag
    const encrypted = new Uint8Array(ciphertextBytes.length + tagBytes.length);
    encrypted.set(ciphertextBytes, 0);
    encrypted.set(tagBytes, ciphertextBytes.length);

    const key = await window.deriveKey(password, Uint8Array.from(atob(salt), c => c.charCodeAt(0)), iterations);

    try {
        const decrypted = await window.crypto.subtle.decrypt(
            { name: "AES-GCM", iv: ivBytes },
            key,
            encrypted
        );
        return new TextDecoder().decode(decrypted);
    } catch (e) {
        return null;
    }
}
window.decryptAesGcm = decryptAesGcm;

window.deriveKey = async function (password, salt, iteration = 100000) {
    const encoder = new TextEncoder();
    const passwordKey = await crypto.subtle.importKey(
        "raw",
        encoder.encode(password),
        { name: "PBKDF2" },
        false,
        ["deriveKey"]
    );
    const keyMaterial = await crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: salt,
            iterations: iteration,
            hash: "SHA-256"
        },
        passwordKey,
        { name: "AES-GCM", length: 256 },
        true,
        ["encrypt", "decrypt"]
    );
    return keyMaterial;
}

window.generateSalt = function (length) {
    const salt = window.crypto.getRandomValues(new Uint8Array(length));
    return btoa(String.fromCharCode(...salt));
}