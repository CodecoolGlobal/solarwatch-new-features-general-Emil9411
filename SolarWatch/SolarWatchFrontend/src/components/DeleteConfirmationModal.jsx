import React, { useEffect} from "react";
import "../design/modal.css";


function DeleteConfirmationModal({ isOpen, handleClose, handleConfirm }){
    useEffect(() => {
        const handleOutsideClick = (event) => {
          if (event.target === document.querySelector(".modal")) {
            handleClose();
          }
        };
    
        if (isOpen) {
          document.addEventListener("mousedown", handleOutsideClick);
        }
    
        return () => {
          document.removeEventListener("mousedown", handleOutsideClick);
        };
      }, [isOpen, handleClose]);
    
      return (
        isOpen && (
          <div className="modal">
            <div className="modal-content">
              <p>Are you sure you want to delete this item?</p>
              <div className="modal-actions">
                <button onClick={handleClose}>Cancel</button>
                <button onClick={handleConfirm}>Confirm</button>
              </div>
            </div>
          </div>
        )
      );
    };

export default DeleteConfirmationModal;
