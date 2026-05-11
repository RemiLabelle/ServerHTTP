import './App.css';
import Graph from "./components/Graph";
import Header from "./components/Header";
import Form from "./components/Form";
import Footer from "./components/Footer";
import {useEffect, useState} from "react";
import Login from "./components/Login";
import Register from "./components/Register";
import gamesClient from "./api/publicationsClient";

function App() {
    const [data, setData] = useState([]);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [auth, setAuth] = useState(true);

    useEffect(() => {
        if (!isLoggedIn) return;

        gamesClient.all().then((response) => {
            const words = response.data;
            setData(words);
        })
        .catch(console.error)
    }, [isLoggedIn]);

    return (
        <div className="App" style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }} >
            <Header/>
                {
                    isLoggedIn ? (
                        <main style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
                            <Graph data={data} />
                            <Form setData={setData} />
                        </main>
                    ) : (
                        <main>
                            {
                                auth ? (
                                    <Login setAuth={setAuth} setIsLoggedIn={setIsLoggedIn} />
                                ) : (
                                    <Register setAuth={setAuth} setIsLoggedIn={setIsLoggedIn} />
                                )
                            }
                        </main>
                    )
                }
            <Footer/>
        </div>
    );
}

export default App;
