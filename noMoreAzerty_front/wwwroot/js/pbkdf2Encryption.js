async function deriveKey(password, salt, iteration) {
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

async function generateSalt(length)
{
    const salt = window.crypto.getRandomValues(new Uint8Array(length));
    return btoa(String.fromCharCode(...salt));
}