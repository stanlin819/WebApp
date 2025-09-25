
document.getElementById("Pagination").addEventListener("click", (e) => {
    e.preventDefault();
    
    let page = e.target.dataset.page;
    if (page < 1 || page > window.totalPages || page == undefined){
        return
    }

    loadPage(page);
})

async function loadPage(page){
    fetch(`/User/UserTablePartial?page=${page}`)
        .then(res => res.text())
        .then(html => {
            document.getElementById("tableContainer").innerHTML = html;
        })
        .catch(error =>{
            console.error("Error loading UserTablePartial", error)
        })
        .finally(()=>{
            console.log("UserTablePartial fetch End");
        })
    
    try{
        const res = await fetch(`/User/Pagination?page=${page}&totalPages=${window.totalPages}`);
        const html = await res.text();
        document.getElementById("Pagination").innerHTML = html;
    }catch (error){
        console.error("Error loading PaginationPartial", error)
    }finally{
        console.log("PaginationPartial End");
    }
    

    // fetch(`/User/Pagination?page=${page}&totalPages=${window.totalPages}`)
    //     .then(res => res.text())
    //     .then(html => {
    //         document.getElementById("Pagination").innerHTML = html;
    //     })
}