import authService from '../components/api-authorization/AuthorizeService';

const authorizeFetch = async (url) => {
    const token = await authService.getAccessToken();
    const options = {
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            ...token ? { 'Authorization': `Bearer ${token}` } : {}
        }
    };
    
    try {
        const response = await fetch(url, options);
        if (response.ok) {
            return await response.json();
        }
    }
    catch (e) {
        console.log(e);
        //toDo обработать ошибку
    }

};

export default authorizeFetch;