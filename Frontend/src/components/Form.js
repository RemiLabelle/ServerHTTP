import {useState} from "react";

export default function Form({ data, setData }) {
    const [answer, setAnswer] = useState("");

    function submit() {
        setData(prevState => {
            const word = prevState.find(w => w.text === answer);
            if (word) {
                return prevState.map(w => w.text === answer ? { ...w, value: w.value + 1 } : w);
            }
            return [...prevState, { text: answer, value: 2 }];
        });
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