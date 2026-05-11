import {useState} from "react";
import gamesClient from "../api/publicationsClient";

export default function Form({ setData }) {
    const [answer, setAnswer] = useState("");

    async function submit() {
        if (answer === "")
            return;

        try {
            await gamesClient.create(answer, '1');
            const res = await gamesClient.all();
            const words = (res.data.data ?? res.data).map(w => ({
                text: w.title,
                value: parseInt(w.content)
            }));
            setData(words);
        } catch (err) {
            console.error(err);
        }

        setAnswer("");
    }

    return (
        <div className="input-group mx-auto mt-4" style={{ maxWidth: '400px' }}>
            <input type="text" placeholder="Entrez votre réponse" className="form-control form-control-lg" onChange={(e) => setAnswer(e.target.value)} value={answer} />
            <button className="btn btn-primary btn-block" type="button" onClick={submit} >
                Envoyer
            </button>
        </div>
    )
}