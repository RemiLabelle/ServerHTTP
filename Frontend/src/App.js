import './App.css';
import Graph from "./components/Graph";
import Header from "./components/Header";
import Form from "./components/Form";
import Footer from "./components/Footer";
import {useState} from "react";

function App() {
    const [data, setData] = useState([]);

    return (
        <div className="App">
            <Header/>
            <Graph data={data} />
            <Form data={data} setData={setData} />
            <Footer/>
        </div>
    );
}

export default App;
