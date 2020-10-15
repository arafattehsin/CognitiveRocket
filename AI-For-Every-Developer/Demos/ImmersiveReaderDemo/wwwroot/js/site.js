// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getImmersiveReaderTokenAsync() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/token',
            type: 'GET',
            success: token => {
                resolve(token);
            },
            error: err => {
                console.log('Error in getting token!', err);
                reject(err);
            }
        });
    });
}

function getImmersiveReaderSubdomainAsync() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/subdomain',
            type: 'GET',
            success: subdomain => {
                resolve(subdomain);
            },
            error: err => {
                console.log('Error in getting subdomain!', err);
                reject(err);
            }
        });
    });
}