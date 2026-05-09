import ReactWordCloud from 'react-d3-cloud'

export default function Graph({ data }) {
    return (
        <main style={{
            width: '100%',
            height: '200',
            display: data.length === 0 ? 'flex' : 'block',
            alignItems: 'center',
            justifyContent: 'center',
        }}>
            {data.length === 0 ? (
                <h2>Les résultats seront affichés ici.</h2>
            ) : (
                <ReactWordCloud
                    data={data}
                    width={800}
                    height={200}
                    font="Fredoka One"
                    fontWeight="bold"
                    fontSize={(word) => {
                        const scale = Math.min(1, 10 / data.length);
                        return (20 + word.value * 5) * scale;
                    }}
                    spiral="rectangular"
                    random={() => 0.5}
                    rotate={(word) => {
                        const maxValue = Math.max(...data.map(w => w.value));
                        return word.value === maxValue ? 0 : (word.text.length % 2 === 0 ? 90 : 0);
                    }}
                />
            )}
        </main>
    )
}