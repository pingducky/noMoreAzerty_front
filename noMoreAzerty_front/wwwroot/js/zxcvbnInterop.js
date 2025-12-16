window.getPasswordStrength = function (password) {
    if (!password) return 0;
    return zxcvbn(password).score * 25;
};