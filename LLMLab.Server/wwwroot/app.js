window.clickLogoutButton = function() {
    // Find the logout button by its ID
    var logoutButton = document.getElementById('logoutButton');
    
    // Check if the button exists
    if (logoutButton) {
        // Simulate a click on the logout button
        logoutButton.click();
    } else {
        console.error('Logout button not found');
    }
}