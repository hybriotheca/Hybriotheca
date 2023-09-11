// #region PROFILE PICTURE EDIT METHODS
//UPLOAD PICTURE
const uploadPhoto = event => {

    hide(fileTypeValidation);

    if (fileExtensionAllowed(photoFile)) {
        profilePicture.src = URL.createObjectURL(event.target.files[0]);

        if (deleteInput != null)
            deleteInput.value = 'False';

        show(deleteButton);
        show(resetButton);
        show(profilePicture);

        hide(profilePictureInput);
    } else {
        photoFile.value = '';
        show(fileTypeValidation);
    }
}

//RESET PICTURE
const resetPhoto = (photoPath) => {
    photoFile.value = null;
    profilePicture.src = photoPath;

    if (deleteInput != null)
        deleteInput.value = 'False';

    show(deleteButton);
    show(profilePicture);

    hide(profilePictureInput);
    hide(resetButton);
}

//DELETE PICTURE
const deletePhoto = () => {
    photoFile.value = null;

    if (deleteInput != null)
        deleteInput.value = 'True';

    show(profilePictureInput);
    show(resetButton);

    hide(deleteButton);
    hide(profilePicture);
}
// #endregion

// #region AUXILIARY FUNCTIONS
function fileExtensionAllowed(file) {
    var fileExtension = file.value.match(/\.(.+)$/)[1];

    switch (fileExtension) {
        case 'jpg':
        case 'jpeg':
        case 'bmp':
        case 'png':
        case 'tif':
            return true;

        default:
            return false;
    }
}

function hide(element) {
    if (element != null)
        element.classList.add('hidden');
}

function show(element) {
    if (element != null)
        element.classList.remove('hidden');
}
// #endregion

// #region COLOR THEME TOGGLE
if (localStorage.getItem('color-theme') === 'dark' || (!('color-theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    document.documentElement.classList.add('dark');
} else {
    document.documentElement.classList.remove('dark')
}

// get button icon
var themeToggleDarkIcon = document.getElementById('theme-toggle-dark-icon');
var themeToggleLightIcon = document.getElementById('theme-toggle-light-icon');

// Change the icons inside the button based on previous settings
if (localStorage.getItem('color-theme') === 'dark' || (!('color-theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    themeToggleLightIcon.classList.remove('hidden');
} else {
    themeToggleDarkIcon.classList.remove('hidden');
}

var themeToggleBtn = document.getElementById('theme-toggle');

themeToggleBtn.addEventListener('click', function () {

    // toggle icons inside button
    themeToggleDarkIcon.classList.toggle('hidden');
    themeToggleLightIcon.classList.toggle('hidden');

    // if set via local storage previously
    if (localStorage.getItem('color-theme')) {
        if (localStorage.getItem('color-theme') === 'light') {
            document.documentElement.classList.add('dark');
            localStorage.setItem('color-theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('color-theme', 'light');
        }

        // if NOT set via local storage previously
    } else {
        if (document.documentElement.classList.contains('dark')) {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('color-theme', 'light');
        } else {
            document.documentElement.classList.add('dark');
            localStorage.setItem('color-theme', 'dark');
        }
    }

});
// #endregion