import React, { useEffect } from "react";
import SwaggerUI from "swagger-ui-react";
import "swagger-ui-react/swagger-ui.css";
import TopBar from "../shared/TopBar";
import Base64Reader from "./Base64Reader";
import copy from "copy-to-clipboard";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const BYTES_PER_MB = 1e6;
const UPLOAD_SIZE_LIMIT = 6 * BYTES_PER_MB;

let yam = require("./api.yaml");

const SwaggerPage = () => {
    useEffect(() => {
        document.addEventListener("input", async (e) => {
            const target = e.target as HTMLInputElement;

            if (!target) {
                return;
            }

            if (target.type === "file") {
                if (!target.files || !target.files.length) {
                    return;
                }

                if (target.files[0].size > UPLOAD_SIZE_LIMIT) {
                    toast.error("The maximum supported request size is 6MB. Please select a smaller file.");
                    target.value = "";
                    return;
                }

                if (target.id === "base64Input") {

                    const base64 = await Base64Reader.ReadFromInput(target);
                    if (copy(base64)) {
                        toast.success("Copied " + target.files[0].name + " as Base64 to clipboard.");
                    }
                }
            }
        });
    }, []);

    return (
        <>
            <TopBar />
            <SwaggerUI url={window.location.origin + yam}
                docExpansion="list"
                defaultModelExpandDepth={1} />
            <ToastContainer />
        </>);
};

export default SwaggerPage;
