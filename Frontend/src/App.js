import './App.css';
import Graph from "./components/Graph";
import Header from "./components/Header";
import Form from "./components/Form";
import Footer from "./components/Footer";
import {useState} from "react";

function App() {
    const [data, setData] = useState([]);

    return (
        <div className="App" style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }} >
            <Header/>
            <main style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
                <Graph data={data} />
                <Form data={data} setData={setData} />
            </main>
            <Footer/>
        </div>
    );
}

export default App;
