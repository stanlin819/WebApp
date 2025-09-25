import { uploadVideo, DeleteVideo } from "./video.js";

const CHUNK_SIZE = 2 * 1024 * 1024;

let currentPage;
let userId;

document.addEventListener('DOMContentLoaded', () => {
  if (window.videoConfig) {
    ({ currentPage, userId} = window.videoConfig); 
  }

});

document.getElementById("uploadBtn").addEventListener("click", () =>{
    uploadVideo(CHUNK_SIZE, userId);
});


document.getElementById("videoList").addEventListener("click", (e)=>{
  if (e.target && e.target.matches("#deleteBtn")){
    e.preventDefault();
    const confirmed = confirm(`確定要刪除檔案 "${e.target.dataset.filename}" 嗎？`);
    if (!confirmed) return;
    DeleteVideo(e.target.dataset.filename, e.target.dataset.userid);
  }
})