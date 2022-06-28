let btnEditFunc = document.querySelector(".btn-edit__func");
let btnLock = document.querySelector(".button-lock");
let btnUnlock = document.querySelector(".button-unlock");
var iconLock = document.getElementById(".icon-lock");
// var btnFunc = document.querySelector(".")
// let searchBtn = document.querySelector(".bx-search");

/*==================== SHOW NAVBAR ====================*/
const showMenu = (headerToggle, navbarId) =>{
    const toggleBtn = document.getElementById(headerToggle),
    nav = document.getElementById(navbarId)
    
    // Validate that variables exist
    if(headerToggle && navbarId){
        toggleBtn.addEventListener('click', ()=>{
            // We add the show-menu class to the div tag with the nav__menu class
            nav.classList.toggle('show-menu')
            // change icon
            toggleBtn.classList.toggle('bx-x')
        })
    }
}
showMenu('header-toggle','navbar')

/*==================== LINK ACTIVE ====================*/
const linkColor = document.querySelectorAll('.nav__link')

function colorLink(){
    linkColor.forEach(l => l.classList.remove('active'))
    this.classList.add('active')
}

linkColor.forEach(l => l.addEventListener('click', colorLink))

const navDropdown = document.querySelectorAll(".nav__dropdown");
for (let i = 0; i < navDropdown.length; i++) {
  navDropdown[i].addEventListener("click", () =>
    navDropdown[i].classList.toggle("open")
  );
}


    // btnUnlock.style.display = "none"
    // btnLock.addEventListener('click', function(){
    //         btnLock.style.display = "none"
    //         btnUnlock.style.display = "inline-block"
    //         document.addEventListener('click', function (e) {
    //             if (e.target.id === btnEditFunc.id) {
    //                 e.preventDefault();
    //             }
    //         });
    //         document.addEventListener('click', function (e){
    //             if(e.target.id === iconLock.id){
    //                 e.preventDefault();
    //             }
    //         })
            
    // })

    // btnUnlock.addEventListener('click', function(){
    //         btnUnlock.style.display = "none"
    //         btnLock.style.display = "inline-block"
    //         // document.addEventListener('click', function (e) {
    //         //     if (e.target.id !== btnEditFunc.id) {
    //         //         e.preventDefault();
    //         //     }
    //         // });
    // }) 


function showPreview(event){
        var file = event.target.files;
        // var src = URL.createObjectURL(event.target.files[1]);
        var preview = document.getElementById("upload-image");
        // preview.file = file;
        preview.style.display = "block";
        console.log('files',file);

        var feedback = document.getElementById('feedback');
        var msg = `${file.length} file(s) tải lên thành công !`;
        feedback.innerHTML = msg;
}