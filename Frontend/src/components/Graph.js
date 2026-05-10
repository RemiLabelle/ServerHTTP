import ReactWordCloud from 'react-d3-cloud'
import { useRef, useEffect, useState } from 'react'

export default function Graph({ data }) {
    const containerRef = useRef(null);
    const [width, setWidth] = useState(800);

    useEffect(() => {
        function updateWidth() {
            if (containerRef.current) {
                setWidth(containerRef.current.offsetWidth);
            }
        }
        updateWidth();
        window.addEventListener('resize', updateWidth);
        return () => window.removeEventListener('resize', updateWidth);
    }, []);

    return (
        <div ref={containerRef} style={{
            width: '100%',
            height: '400px',
            flexShrink: 0,
            display: data.length === 0 ? 'flex' : 'block',
            alignItems: 'center',
            justifyContent: 'center',
            position: 'relative',
            right: data.length === 0 ? '0%' : '1%',
        }}>
            {data.length === 0 ? (
                <h2>Les résultats seront affichés ici.</h2>
            ) : (
                <ReactWordCloud
                    data={data}
                    width={width}
                    height={400}
                    font="Fredoka One"
                    fontWeight="bold"
                    fontSize={(word) => {
                        const maxValue = Math.max(...data.map(w => w.value));
                        const normalized = word.value / maxValue;
                        return 12 + normalized * 68;
                    }}
                    spiral="rectangular"
                    random={() => 0.5}
                    rotate={(word) => {
                        const maxValue = Math.max(...data.map(w => w.value));
                        return word.value === maxValue ? 0 : (word.text.length % 2 === 0 ? 90 : 0);
                    }}
                />
            )}
        </div>
    )
}