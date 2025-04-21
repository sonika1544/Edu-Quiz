// Password validation functions
function validatePassword(password) {
    return {
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /[0-9]/.test(password),
        special: hasSpecialChar(password)
    };
}

function hasSpecialChar(str) {
    var specialChars = '!@#$%^&*()_+-=[]{}|;:,.<>?';
    for (var i = 0; i < str.length; i++) {
        if (specialChars.indexOf(str[i]) !== -1) {
            return true;
        }
    }
    return false;
}

function updatePasswordValidation(password, updateUI) {
    var validations = validatePassword(password);
    var isValid = true;

    // Update each validation check
    for (var key in validations) {
        if (validations.hasOwnProperty(key)) {
            updateUI(key + '-check', validations[key]);
            isValid = isValid && validations[key];
        }
    }

    return isValid;
}

function calculatePasswordStrength(password) {
    let strength = 0;
    
    // Length check
    if (password.length >= 8) strength += 25;
    
    // Contains number
    if (/\d/.test(password)) strength += 25;
    
    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 25;
    
    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 25;
    
    return strength;
}

function updatePasswordStrengthIndicator(progressBar, strength) {
    progressBar.style.width = strength + '%';
    
    if (strength < 25) {
        progressBar.className = 'progress-bar bg-danger';
    } else if (strength < 50) {
        progressBar.className = 'progress-bar bg-warning';
    } else if (strength < 75) {
        progressBar.className = 'progress-bar bg-info';
    } else {
        progressBar.className = 'progress-bar bg-success';
    }
} 