import React, { useEffect } from "react";
import "../design/modal.css";

function MessageModal({ isOpen, message, handleClose, handleConfirm }) {
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
          <p>{message}</p>
          <div className="modal-actions">
            <button onClick={handleClose}>Close</button>
            {handleConfirm && <button onClick={handleConfirm}>Confirm</button>}
          </div>
        </div>
      </div>
    )
  );
}

export default MessageModal;
