import {useState} from "react";
import authClient from "../api/authClient";

export default function Register({ setAuth, setIsLoggedIn }) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState(false);

    async function register() {
        await authClient.register(username, password).then((response) => {
            setIsLoggedIn(true);
            console.log(response);
        })
        .catch(() => setError(true));
    }

    return (
        <div className="container d-flex flex-column justify-content-center align-items-center vh-100">
            <span className="card bg-danger text-white my-2" style={{ width: '380px' }} hidden={!error}>
                {
                    username.length === 0 || password.length === 0 ? (
                        <>Certains champs n'ont pas de saisie</>
                    ) : (
                        <>Veuillez réessayer plus tard</>
                    )
                }
            </span>
            <div className="card p-4 shadow" style={{ width: '380px' }}>
                <h3 className="text-center mb-4 fw-bold">Inscription</h3>
                <div className="mb-3">
                    <input type="text" className="form-control" placeholder="Nom d'utilisateur" value={username} onChange={(e) => { setUsername(e.target.value); setError(false) }}/>
                </div>
                <div className="mb-3">
                    <input type="password" className="form-control" placeholder="Mot de passe" value={password} onChange={(e) => { setPassword(e.target.value); setError(false) }}/>
                </div>
                <button className="btn btn-primary w-100" onClick={register}>S'enregistrer</button>
                <a className="py-1" style={{ cursor: 'pointer' }} onClick={() => setAuth(true)}>Se connecter</a>
            </div>
        </div>
    );
}